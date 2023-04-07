#ifndef clox_integer_h
#define clox_integer_h

#include "common.h"

typedef struct {
    int capacity;
    int count;
    int* values;
} IntegerArray;

void initIntegerArray(IntegerArray* array);
void writeIntegerArray(IntegerArray* array, int value);
void freeIntegerArray(IntegerArray* array);

#endif