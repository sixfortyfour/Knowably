<script setup lang="ts">
import { onMounted } from 'vue'
import FileDropZone from '@/components/FileDropZone.vue'
import DocumentLibraryList from '@/components/DocumentLibraryList.vue'
import { useDocumentsStore } from '@/stores/documents'

const store = useDocumentsStore()

onMounted(() => store.fetchDocuments())

async function onFileSelected(file: File) {
  try {
    await store.upload(file)
  } catch {
    // error handled in store
  }
}
</script>

<template>
  <div class="space-y-8">
    <div>
      <h1 class="text-2xl font-bold text-gray-900">Upload Documents</h1>
      <p class="mt-1 text-sm text-gray-500">Upload PDF, TXT, or Markdown files to build your knowledge base.</p>
    </div>

    <FileDropZone @file-selected="onFileSelected" />

    <div>
      <h2 class="text-base font-semibold text-gray-700 mb-3">Document Library</h2>
      <DocumentLibraryList />
    </div>
  </div>
</template>
