<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <nav class="bg-white border-b border-gray-200 shadow-sm">
      <div class="max-w-3xl mx-auto px-4 sm:px-6 flex items-center justify-between h-14">
        <span class="text-base font-bold text-indigo-600 tracking-tight">RagQnA</span>
        <div class="flex gap-1">
          <RouterLink
            to="/upload"
            class="rounded-lg px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-100 transition-colors"
            active-class="bg-indigo-50 text-indigo-700"
          >
            Upload
          </RouterLink>
          <RouterLink
            to="/ask"
            class="rounded-lg px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-100 transition-colors"
            active-class="bg-indigo-50 text-indigo-700"
          >
            Ask
          </RouterLink>
        </div>
      </div>
    </nav>
    <main class="max-w-3xl mx-auto px-4 sm:px-6 py-10">
      <RouterView />
    </main>
  </div>
</template>
