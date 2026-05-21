<script setup lang="ts">
import type { QuestionResponse } from '@/api/types'

defineProps<{ response: QuestionResponse }>()
</script>

<template>
  <div class="rounded-xl border border-gray-200 bg-white p-5 shadow-sm space-y-3">
    <div class="flex items-start justify-between gap-2">
      <p class="text-sm font-medium text-gray-500 italic">"{{ response.question }}"</p>
      <div class="flex items-center gap-2 shrink-0">
        <span
          v-if="response.cached"
          class="inline-flex items-center rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-700"
        >
          From cache
        </span>
        <span class="text-xs text-gray-400">{{ response.durationMs }}ms</span>
      </div>
    </div>
    <p class="text-sm text-gray-800 leading-relaxed whitespace-pre-wrap">{{ response.answer }}</p>
  </div>
</template>
