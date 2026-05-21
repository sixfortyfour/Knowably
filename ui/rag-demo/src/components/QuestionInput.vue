<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{ loading: boolean }>()
const emit = defineEmits<{ (e: 'ask', question: string): void }>()

const question = ref('')

function submit() {
  const q = question.value.trim()
  if (!q || props.loading) return
  emit('ask', q)
  question.value = ''
}
</script>

<template>
  <div class="flex gap-2">
    <textarea
      v-model="question"
      rows="3"
      placeholder="Ask a question about your documents…"
      class="flex-1 resize-none rounded-xl border border-gray-300 px-4 py-3 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 disabled:opacity-50"
      :disabled="loading"
      @keydown.enter.meta.prevent="submit"
      @keydown.enter.ctrl.prevent="submit"
    />
    <button
      class="self-end rounded-xl bg-indigo-600 px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      :disabled="loading || !question.trim()"
      @click="submit"
    >
      <span v-if="loading">Thinking…</span>
      <span v-else>Ask</span>
    </button>
  </div>
</template>
