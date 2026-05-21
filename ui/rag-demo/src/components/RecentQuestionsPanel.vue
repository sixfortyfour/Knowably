<script setup lang="ts">
import type { QuestionResponse } from '@/api/types'

defineProps<{ history: QuestionResponse[] }>()
const emit = defineEmits<{ (e: 'reask', question: string): void }>()
</script>

<template>
  <div v-if="history.length > 0" class="space-y-2">
    <h3 class="text-xs font-semibold uppercase tracking-wide text-gray-400">Recent questions</h3>
    <button
      v-for="item in history"
      :key="item.question"
      class="block w-full text-left rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700 hover:border-indigo-300 hover:bg-indigo-50 transition-colors"
      @click="emit('reask', item.question)"
    >
      <span class="line-clamp-1">{{ item.question }}</span>
    </button>
  </div>
</template>
