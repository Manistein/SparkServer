CSERVICE_DIR = ../../clib/loggerx
CSER_SRC_DIR = .

uname_S := $(shell sh -c 'uname -s 2>/dev/null || echo not')
ifeq ($(uname_S), Darwin)
	SHARED = -fPIC
	CFLAGS = -O3 -Wall -pedantic -DNDEBUG 
	CLOG_LDFLAGS = -bundle -undefined dynamic_lookup
else
	SHARED = -fPIC --shared
	CFLAGS = -g -O2 -Wall 	
endif

# cservice-src
CSER_INCLUDE_DIR = ../../../skynet/skynet-src
CSER_LOGGER_TARGET = $(CSERVICE_DIR)/loggerx.so

all: $(CSER_LOGGER_TARGET)

.PHONY: all clean

.c.o:
	$(CC) -c $(SHARED) $(CFLAGS) -o $@ $<

$(CSER_LOGGER_TARGET): $(CSER_SRC_DIR)/service_logger.c
ifeq ($(uname_S), Darwin)
	$(CC) $(LDFLAGS) $(CLOG_LDFLAGS) -I$(CSER_INCLUDE_DIR) $^ -o $@
else
	$(CC) $(SHARED) $(CFLAGS) -I$(CSER_INCLUDE_DIR) $^ -o $@ 
endif

clean:
	rm -f $(CSERVICE_DIR)/*.so
