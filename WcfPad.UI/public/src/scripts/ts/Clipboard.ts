class Clipboard {
    copy(text: string) {
        var textArea = document.createElement("textarea");

        // needs width & height to be selectable
        textArea.style.width = "1";
        textArea.style.height = "1";

        textArea.style.position = "fixed";
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.padding = "0";
        textArea.style.opacity = "0";
        textArea.style.border = "none";
        textArea.style.outline = "none";
        textArea.style.boxShadow = "none";
        textArea.style.background = "transparent";
        
        textArea.value = text;
        document.body.appendChild(textArea);

        textArea.select();
        document.execCommand('copy');

        document.body.removeChild(textArea);
    }
}