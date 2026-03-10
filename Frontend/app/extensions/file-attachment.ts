import { Node, mergeAttributes } from '@tiptap/core'
import type { JSONContent } from '@tiptap/core'
import { VueNodeViewRenderer } from '@tiptap/vue-3'
import FileAttachmentCard from '~/components/FileAttachmentCard.vue'

// Export a factory so each MarkdownEditor component instance gets its own
// extension object with unique ProseMirror plugin keys, avoiding the
// "Adding different instances of a keyed plugin" error.
export const FileAttachmentExtension = Node.create({
  name: 'fileAttachment',
  group: 'block',
  atom: true,
  draggable: true,

  addAttributes() {
    return {
      src: { default: null },
      fileName: { default: '' },
      fileSize: { default: 0 },
      contentType: { default: '' }
    }
  },

  parseHTML() {
    return [
      {
        tag: 'div[data-type="file-attachment"]',
        getAttrs: (element) => {
          const el = element as HTMLElement
          return {
            src: el.getAttribute('data-src'),
            fileName: el.getAttribute('data-file-name') || '',
            fileSize: Number(el.getAttribute('data-file-size')) || 0,
            contentType: el.getAttribute('data-content-type') || ''
          }
        }
      }
    ]
  },

  renderHTML({ node }) {
    return [
      'div',
      mergeAttributes({
        'data-type': 'file-attachment',
        'data-src': node.attrs.src,
        'data-file-name': node.attrs.fileName,
        'data-file-size': String(node.attrs.fileSize),
        'data-content-type': node.attrs.contentType
      })
    ]
  },

  renderMarkdown(node: JSONContent) {
    const fileName = String(node.attrs?.['fileName'] ?? '')
    const src = String(node.attrs?.['src'] ?? '')
    const contentType = String(node.attrs?.['contentType'] ?? '')
    const fileSize = Number(node.attrs?.['fileSize'] ?? 0)
    // Emit raw HTML so markdown-it passes it through unchanged and parseHTML
    // rules can reconstruct the fileAttachment node on load.
    return `<div data-type="file-attachment" data-src="${src}" data-file-name="${fileName}" data-file-size="${fileSize}" data-content-type="${contentType}"></div>`
  },

  addNodeView() {
    return VueNodeViewRenderer(FileAttachmentCard)
  }
})
