<script setup lang="ts">
defineProps<{
  modelValue: string
  label?: string
  placeholder?: string
  type?: string
  autocomplete?: string
  maxlength?: number
}>()

defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()
</script>

<template>
  <div class="flex flex-col gap-1">
    <label v-if="label" class="font-pixel text-[8px] text-murder-dim uppercase tracking-wider">
      {{ label }}
    </label>
    <input
      :type="type ?? 'text'"
      :value="modelValue"
      :placeholder="placeholder"
      :autocomplete="autocomplete"
      :maxlength="maxlength"
      class="bg-murder-surface border-2 border-murder-border text-murder-text font-body text-sm px-3 py-2 outline-none focus:border-murder-accent placeholder:text-murder-dim w-full"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
  </div>
</template>
