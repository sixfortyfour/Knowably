<script setup lang="ts">
import { ref } from 'vue'
import type { SourceChunk } from '@/api/types'

defineProps<{ chunks: SourceChunk[] }>()

const expanded = ref(false)
</script>

<template>
  <div class="rounded-xl border border-gray-200 bg-gray-50">
    <button
      class="flex w-full items-center justify-between px-4 py-3 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-xl transition-colors"
      @click="expanded = !expanded"
    >
      <span>Source chunks ({{ chunks.length }})</span>
      <svg
        class="w-4 h-4 text-gray-400 transition-transform"
        :class="expanded ? 'rotate-180' : ''"
        fill="none" stroke="currentColor" viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>
    <div v-if="expanded" class="border-t border-gray-200 divide-y divide-gray-100">
      <div v-for="(chunk, i) in chunks" :key="i" class="px-4 py-3 space-y-1">
        <div class="flex items-center gap-2">
          <span class="text-xs font-mono text-gray-400">#{{ chunk.chunkIndex }}</span>
          <span class="text-xs text-gray-400">doc {{ chunk.documentId.slice(0, 8) }}…</span>
          <span class="ml-auto text-xs font-medium text-indigo-600">{{ (chunk.score * 100).toFixed(1) }}%</span>
        </div>
        <p class="text-xs text-gray-600 leading-relaxed line-clamp-4">{{ chunk.text }}</p>
      </div>
    </div>
  </div>
</template>
