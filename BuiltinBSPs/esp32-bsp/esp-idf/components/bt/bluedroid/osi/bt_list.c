
#include "bt_defs.h"

#include "allocator.h"
#include "bt_list.h"
#include "osi.h"

struct list_node_t {
    struct list_node_t *next;
    void *data;
};

typedef struct list_t {
    list_node_t *head;
    list_node_t *tail;
    size_t length;
    list_free_cb free_cb;
    const allocator_t *allocator;
} list_t;

//static list_node_t *list_free_node_(list_t *list, list_node_t *node);

// Hidden constructor, only to be used by the hash map for the allocation tracker.
// Behaves the same as |list_new|, except you get to specify the allocator.
list_t *list_new_internal(list_free_cb callback, const allocator_t *zeroed_allocator)
{
    list_t *list = (list_t *)zeroed_allocator->alloc(sizeof(list_t));
    if (!list) {
        return NULL;
    }

    list->head = list->tail = NULL;
    list->length = 0;
    list->free_cb = callback;
    list->allocator = zeroed_allocator;
    return list;
}

list_t *list_new(list_free_cb callback)
{
    return list_new_internal(callback, &allocator_calloc);
}

void list_free(list_t *list)
{
    if (!list) {
        return;
    }

    list_clear(list);
    list->allocator->free(list);
}

bool list_is_empty(const list_t *list)
{
    assert(list != NULL);
    return (list->length == 0);
}

/*
bool list_contains(const list_t *list, const void *data) {
  const list_node_t *node;
  assert(list != NULL);
  assert(data != NULL);

  for (node = list_begin(list); node != list_end(list); node = list_next(node)) {
    if (list_node(node) == data)
      return true;
  }

  return false;
}
*/

size_t list_length(const list_t *list)
{
    assert(list != NULL);
    return list->length;
}

void *list_front(const list_t *list)
{
    assert(list != NULL);
    assert(!list_is_empty(list));

    return list->head->data;
}

/*
void *list_back(const list_t *list) {
  assert(list != NULL);
  assert(!list_is_empty(list));

  return list->tail->data;
}
*/

bool list_insert_after(list_t *list, list_node_t *prev_node, void *data)
{
    list_node_t *node;
    assert(list != NULL);
    assert(prev_node != NULL);
    assert(data != NULL);

    node = (list_node_t *)list->allocator->alloc(sizeof(list_node_t));
    if (!node) {
        return false;
    }

    node->next = prev_node->next;
    node->data = data;
    prev_node->next = node;
    if (list->tail == prev_node) {
        list->tail = node;
    }
    ++list->length;
    return true;
}

bool list_prepend(list_t *list, void *data)
{
    list_node_t *node;
    assert(list != NULL);
    assert(data != NULL);

    node = (list_node_t *)list->allocator->alloc(sizeof(list_node_t));
    if (!node) {
        return false;
    }
    node->next = list->head;
    node->data = data;
    list->head = node;
    if (list->tail == NULL) {
        list->tail = list->head;
    }
    ++list->length;
    return true;
}

bool list_append(list_t *list, void *data)
{
    list_node_t *node;
    assert(list != NULL);
    assert(data != NULL);

    node = (list_node_t *)list->allocator->alloc(sizeof(list_node_t));
    if (!node) {
        return false;
    }
    node->next = NULL;
    node->data = data;
    if (list->tail == NULL) {
        list->head = node;
        list->tail = node;
    } else {
        list->tail->next = node;
        list->tail = node;
    }
    ++list->length;
    return true;
}

bool list_remove(list_t *list, void *data)
{
    assert(list != NULL);
    assert(data != NULL);

    if (list_is_empty(list)) {
        return false;
    }

    if (list->head->data == data) {
        list_node_t *next = list_free_node(list, list->head);
        if (list->tail == list->head) {
            list->tail = next;
        }
        list->head = next;
        return true;
    }

    for (list_node_t *prev = list->head, *node = list->head->next; node; prev = node, node = node->next)
        if (node->data == data) {
            prev->next = list_free_node(list, node);
            if (list->tail == node) {
                list->tail = prev;
            }
            return true;
        }

    return false;
}

void list_clear(list_t *list)
{
    assert(list != NULL);
    for (list_node_t *node = list->head; node; ) {
        node = list_free_node(list, node);
    }
    list->head = NULL;
    list->tail = NULL;
    list->length = 0;
}

void list_foreach(const list_t *list, list_iter_cb callback)
{
    assert(list != NULL);
    assert(callback != NULL);

    for (list_node_t *node = list->head; node; ) {
        list_node_t *next = node->next;
        callback(node->data);
        node = next;
    }
}

list_node_t *list_begin(const list_t *list)
{
    assert(list != NULL);
    return list->head;
}

list_node_t *list_end(UNUSED_ATTR const list_t *list)
{
    assert(list != NULL);
    return NULL;
}

list_node_t *list_next(const list_node_t *node)
{
    assert(node != NULL);
    return node->next;
}

void *list_node(const list_node_t *node)
{
    assert(node != NULL);
    return node->data;
}

list_node_t *list_free_node(list_t *list, list_node_t *node)
{
    assert(list != NULL);
    assert(node != NULL);

    list_node_t *next = node->next;

    if (list->free_cb) {
        list->free_cb(node->data);
    }
    list->allocator->free(node);
    --list->length;

    return next;
}
