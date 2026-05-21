import { createRouter, createWebHistory } from 'vue-router'
import DocumentUploadView from '@/views/DocumentUploadView.vue'
import QuestionAnswerView from '@/views/QuestionAnswerView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/upload' },
    { path: '/upload', component: DocumentUploadView },
    { path: '/ask', component: QuestionAnswerView },
  ],
})

export default router
