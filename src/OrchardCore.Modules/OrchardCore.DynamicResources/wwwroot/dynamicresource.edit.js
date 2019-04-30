var editor;

function initializeResourceEditor(contentTypeElement, editorElement) {
    

    editor = CodeMirror.fromTextArea(editorElement, {
        autoRefresh: true,
        lineNumbers: true,
        styleActiveLine: true,
        matchBrackets: true,
        mode: { name: contentTypeElement.value },
    });



}