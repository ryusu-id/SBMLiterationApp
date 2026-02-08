<script setup lang="ts">
import type { DailyRead } from './DailyReadsTable.vue'

defineProps<{
  dailyRead: DailyRead
}>()

function navigateToDaily(id: string) {
  useRouter().push({ name: 'daily-slug', params: { slug: id } })
}
</script>

<template>
  <div
    class="relative w-full h-full border-5 border-[#3566CD] rounded-[36px] overflow-hidden bg-cover bg-center cursor-pointer transition-all duration-300 hover:shadow-2xl active:scale-95 group bg-muted"
    :style="dailyRead.coverImg ? `background-image: url('${dailyRead.coverImg}')` : 'background: linear-gradient(135deg, #667eea 0%, #764ba2 100%)'"
    @click="navigateToDaily(dailyRead.id)"
  >
    <div class="absolute inset-0 bg-gradient-to-t from-black/80 via-black/40 to-black/20 group-hover:from-black/90 transition-all duration-300" />

    <div class="relative h-full flex flex-col justify-end p-6 text-white">
      <div class="space-y-2">
        <div class="flex items-center gap-2 text-xs opacity-90">
          <UIcon
            name="i-lucide-calendar"
            class="size-4"
          />
          <span>{{ new Date(dailyRead.date).toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</span>
        </div>

        <h2 class="text-2xl font-bold leading-tight line-clamp-3">
          {{ dailyRead.title }}
        </h2>

        <div
          v-if="dailyRead.category"
          class="flex items-center gap-2"
        >
          <UBadge
            color="primary"
            variant="subtle"
            size="sm"
          >
            {{ dailyRead.category }}
          </UBadge>
        </div>

        <div class="flex items-center gap-4 text-sm pt-2">
          <div class="flex items-center gap-1">
            <UIcon
              name="i-lucide-star"
              class="size-4"
            />
            <span>{{ dailyRead.exp }} EXP</span>
          </div>
        </div>
      </div>

      <div class="mt-4 pt-4 border-t border-white/20">
        <div class="flex items-center justify-between text-sm">
          <span class="opacity-80">Click to read</span>
          <UIcon
            name="i-heroicons-arrow-right"
            class="size-5 group-hover:translate-x-1 transition-transform"
          />
        </div>
      </div>
    </div>
  </div>
</template>
