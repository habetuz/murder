<script setup lang="ts">
withDefaults(defineProps<{
  variant?: 'primary' | 'danger' | 'ghost' | 'warn' | 'code'
  size?: 'sm' | 'md' | 'lg'
  disabled?: boolean
  loading?: boolean
  type?: 'button' | 'submit' | 'reset'
}>(), {
  variant: 'primary',
  size: 'md',
  disabled: false,
  loading: false,
  type: 'button',
})
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'font-pixel border-2 cursor-pointer transition-all duration-100 select-none',
      'disabled:opacity-50 disabled:cursor-not-allowed disabled:shadow-none disabled:translate-x-0 disabled:translate-y-0',
      'active:translate-x-[2px] active:translate-y-[2px] active:shadow-none',
      // Sizes
      size === 'sm' && 'text-[8px] px-3 py-2 shadow-pixel-sm',
      size === 'md' && 'text-[9px] px-4 py-3 shadow-pixel',
      size === 'lg' && 'text-[10px] px-6 py-4 shadow-pixel',
      // Variants
      variant === 'primary' && 'bg-murder-accent text-murder-bg border-murder-accent hover:brightness-110 shadow-pixel',
      variant === 'danger' && 'bg-murder-danger text-white border-murder-danger hover:brightness-110 shadow-pixel',
      variant === 'ghost' && 'bg-transparent text-murder-text border-murder-border hover:border-murder-accent hover:text-murder-accent shadow-pixel',
      variant === 'warn' && 'bg-murder-warn text-murder-bg border-murder-warn hover:brightness-110 shadow-pixel',
      variant === 'code' && 'bg-murder-surface text-murder-code border-murder-code hover:bg-murder-surface-light shadow-pixel',
    ]"
  >
    <span v-if="loading" class="inline-block animate-pulse">...</span>
    <slot v-else />
  </button>
</template>
