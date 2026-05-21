<script setup lang="ts">
import type { DocumentListItem } from '@/api/types'

defineProps<{ doc: DocumentListItem }>()

const statusLabel: Record<string, string> = {
  Pending: 'Queued',
  Indexing: 'Indexing…',
  Indexed: 'Ready',
  Failed: 'Failed',
}

const statusClass: Record<string, string> = {
  Pending: 'bg-gray-100 text-gray-600',
  Indexing: 'bg-amber-100 text-amber-700',
  Indexed: 'bg-green-100 text-green-700',
  Failed: 'bg-red-100 text-red-700',
}
</script>

<template>
  <div class="flex items-center justify-between rounded-lg border border-gray-200 bg-white px-4 py-3 shadow-sm">
    <div class="flex items-center gap-3 min-w-0">
      <svg class="w-5 h-5 shrink-0 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
          d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
      </svg>
      <span class="truncate text-sm font-medium text-gray-800">{{ doc.fileName }}</span>
    </div>
    <div class="flex items-center gap-3 shrink-0 ml-4">
      <span v-if="doc.chunkCount !== null" class="text-xs text-gray-400">{{ doc.chunkCount }} chunks</span>
      <span
        class="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
        :class="statusClass[doc.status] ?? 'bg-gray-100 text-gray-600'"
      >
        {{ statusLabel[doc.status] ?? doc.status }}
      </span>
    </div>
  </div>
</template>
