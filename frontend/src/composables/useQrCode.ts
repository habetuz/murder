import { ref, watch, isRef, type MaybeRef } from 'vue'
import QRCode from 'qrcode'

export function useQrCode(text: MaybeRef<string>) {
  const dataUrl = ref('')

  async function generate(value: string) {
    if (!value) {
      dataUrl.value = ''
      return
    }
    try {
      dataUrl.value = await QRCode.toDataURL(value, {
        width: 220,
        margin: 2,
        color: {
          dark: '#e8e0f0',
          light: '#2d1b4e',
        },
      })
    } catch {
      dataUrl.value = ''
    }
  }

  const val = isRef(text) ? text : ref(text)
  watch(val, generate, { immediate: true })

  return { dataUrl }
}
