window.QuillHelpers = {
    getEditorHTML: function (wrapperId) {
        var container = document.querySelector('#' + wrapperId + ' .ql-container');
        if (!container || !container.__quill) {
            return '';
        }

        return container.__quill.getText().trim().length === 0
            ? ''
            : container.__quill.root.innerHTML;
    },
    setEditorHTML: function (wrapperId, html) {
        var container = document.querySelector('#' + wrapperId + ' .ql-container');
        if (container && container.__quill) {
            var delta = container.__quill.clipboard.convert(html || '');
            container.__quill.setContents(delta, 'silent');
        }
    },
    setEditorHeight: function (wrapperId, height) {
        var editor = document.querySelector('#' + wrapperId + ' .ql-editor');
        if (editor) {
            editor.style.minHeight = Math.max(Number(height) || 0, 100) + 'px';
        }
    }
};
