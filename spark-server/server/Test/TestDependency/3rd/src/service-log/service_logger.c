#include "skynet.h"
#include "skynet_timer.h"

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <libgen.h>
#include <unistd.h>

#define BUFF_SIZE 1024
#define MAX_LOG_LINE_COUNT 655350

struct logger {
    char date[BUFF_SIZE];
    char folder_name[BUFF_SIZE];
    char folder_path[BUFF_SIZE];
    FILE* handle;
    int close;
    int current_line_count;
};

struct logger *
loggerx_create(void) {
    struct logger * inst = skynet_malloc(sizeof(*inst));
    memset(inst, 0, sizeof(*inst));
    return inst;
}

void
loggerx_release(struct logger * inst) {
    if (inst->close) {
        fclose(inst->handle);
    }
    skynet_free(inst);
}

static void 
format_date(char *buf, int len) {
    double now = skynet_starttime() + skynet_now() * 0.01;

    time_t sec = (time_t)now;

    strftime(buf, len, "%F", localtime(&sec));
}

static void 
format_filename(char *buf, int len) {
    double now = skynet_starttime() + skynet_now() * 0.01;

    time_t sec = (time_t)now;

    strftime(buf, len, "%Y-%m-%d-%H-%M-%S", localtime(&sec));
}

static void 
format_time(char *buf, int len) {
    double now = skynet_starttime() + skynet_now() * 0.01;

    time_t sec = (time_t)now;
    int ms = (now - sec) * 100;

    char tmp_time[BUFF_SIZE];
    strftime(tmp_time, BUFF_SIZE, "%F %T", localtime(&sec));

    snprintf(buf, len, "%s.%02d", tmp_time, ms);
}

static void 
create_folder(char *path)
{
    char *subpath, *fullpath, *dirpath;

    fullpath = strdup(path);
    dirpath = strdup(path);
    subpath = dirname(dirpath);
    if (strlen(subpath) > 1)
        create_folder(subpath);
    
    struct stat st = {0};
    if (stat(fullpath, &st) == -1) {
        mkdir(fullpath, 0700);
    }
    free(fullpath);
    free(dirpath);
    // free(subpath);
}

static int 
try_new_file(void *ud) {
    struct logger * inst = ud;

    char date[BUFF_SIZE];
    format_date(date, BUFF_SIZE);

    if (strcmp(inst->date, date) != 0 || inst->current_line_count >= MAX_LOG_LINE_COUNT ) {
        if (inst->handle != NULL) {
            fclose(inst->handle);
            inst->handle = NULL;
        }

        inst->current_line_count = 0;
        memcpy(inst->date, date, BUFF_SIZE);

        // file name
        char filename[BUFF_SIZE];
        char current_time[BUFF_SIZE];
        format_filename(current_time, BUFF_SIZE);
        snprintf(filename, BUFF_SIZE, "%s-%s.log", inst->folder_name, current_time);

        // folder
        char folderpath[BUFF_SIZE];
        memset(folderpath, 0, BUFF_SIZE);
        snprintf(folderpath, BUFF_SIZE, "%s/%s", inst->folder_path, inst->date);
        create_folder(folderpath);

        char filepath[BUFF_SIZE];
        memset(filepath, 0, BUFF_SIZE);
        snprintf(filepath, BUFF_SIZE, "%s/%s", folderpath, filename);

        inst->handle = fopen(filepath,"a+");
        if (inst->handle == NULL) {
            inst->close = 0;
            fprintf(stderr, "can't open file: %s\n", filepath);
            return 1;
        }
    }
    return 0;
}

static int
_logger(struct skynet_context * context, void *ud, int type, int session, uint32_t source, const void * msg, size_t sz) {
    struct logger * inst = ud;

    if (inst->handle == NULL) {
        fprintf(stderr, "write log fail: \n");
        return 1;
    }
    if (inst->handle != stdout) {
        try_new_file(inst);
        inst->current_line_count++;
    }
    char time_str[BUFF_SIZE] = {0};
    format_time(time_str, BUFF_SIZE);

    fprintf(inst->handle, "%s", time_str);
    fprintf(inst->handle, ":[%08x] ",source);
    fwrite(msg, sz , 1, inst->handle);
    fprintf(inst->handle, "\n");
    fflush(inst->handle);

    return 0;
}

int
loggerx_init(struct logger * inst, struct skynet_context *ctx, const char * parm) {
    if (parm) {
        snprintf(inst->folder_path, BUFF_SIZE, "%s", parm);
        char * folder_end_str = basename(inst->folder_path);
        memcpy(inst->folder_name, folder_end_str, strlen(folder_end_str));
        inst->folder_name[strlen(folder_end_str)]='\0';
        free(folder_end_str);
    } 

    if (strlen(inst->folder_path) != 0) {
        inst->handle = NULL;

        try_new_file(inst);
    } else {
        inst->handle = stdout;
    }

    if (inst->handle) {
        skynet_callback(ctx, inst, _logger);
        skynet_command(ctx, "REG", ".logger");
        return 0;
    }
    return 1;
}
