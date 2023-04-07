#include "integer.h"
#include "memory.h"

void initIntegerArray(IntegerArray* array) {
    array->values = NULL;
    array->capacity = 0;
    array->count = 0;
}

void writeIntegerArray(IntegerArray* array, int value) {
    if(array->capacity < array->count + 1) {
        int oldCapacity = array->capacity;
        array->capacity = GROW_CAPACITY(oldCapacity);
        array->values = GROW_ARRAY(int, array->values, oldCapacity, array->capacity);
    }

    array->values[array->count] = value;
    array->count++;
}

void freeIntegerArray(IntegerArray* array) {
    FREE_ARRAY(int, array->values, array->capacity);
    initIntegerArray(array);
}