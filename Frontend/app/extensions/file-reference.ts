import { Mark, mergeAttributes } from '@tiptap/core'
import type { JSONContent } from '@tiptap/core'

export interface FileReferenceAttributes {
  href: string | null
  fileName: string
  fileSize: number
  contentType: string
}

/**
 * Inline Mark extension that turns selected text into a styled file reference link.
 * Use this to link arbitrary text to an uploaded file, in contrast to the block-level
 * FileAttachmentExtension (which renders the full FileAttachmentCard).
 *
 * Markdown round-trip: `[selected text](href "file-ref:fileName")`
 */
export const FileReferenceExtension = Mark.create<FileReferenceAttributes>({
  name: 'fileReference',
  priority: 1000,

  addAttributes() {
    return {
      href: { default: null },
      fileName: { default: '' },
      fileSize: { default: 0 },
      contentType: { default: '' }
    }
  },

  parseHTML() {
    return [
      {
        tag: 'a[data-type="file-reference"]',
        getAttrs: (element) => {
          const el = element as HTMLElement
          return {
            href: el.getAttribute('href'),
            fileName: el.getAttribute('data-file-name') || '',
            fileSize: Number(el.getAttribute('data-file-size')) || 0,
            contentType: el.getAttribute('data-content-type') || ''
          }
        }
      }
    ]
  },

  renderHTML({ HTMLAttributes }) {
    return [
      'a',
      mergeAttributes({
        'data-type': 'file-reference',
        'href': HTMLAttributes.href,
        'data-file-name': HTMLAttributes.fileName,
        'data-file-size': String(HTMLAttributes.fileSize),
        'data-content-type': HTMLAttributes.contentType,
        'class': 'file-reference-mark inline-flex items-center gap-1 text-primary underline cursor-pointer',
        'target': '_blank',
        'rel': 'noopener noreferrer'
      }),
      0 // 0 = render text children inside the element
    ]
  },

  // Markdown serialiser: emits raw HTML so markdown-it passes it through unchanged
  // and parseHTML rules reconstruct the fileReference mark on load.
  renderMarkdown(node: JSONContent) {
    const src = String(node.attrs?.['href'] ?? '')
    const fileName = String(node.attrs?.['fileName'] ?? '')
    const fileSize = String(node.attrs?.['fileSize'] ?? 0)
    const contentType = String(node.attrs?.['contentType'] ?? '')
    const text = String(node.text ?? '')
    return `<a data-type="file-reference" href="${src}" data-file-name="${fileName}" data-file-size="${fileSize}" data-content-type="${contentType}">${text}</a>`
  }
})
