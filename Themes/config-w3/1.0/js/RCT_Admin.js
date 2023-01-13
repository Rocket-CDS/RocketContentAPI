rctadmin = function () {


    function CopyTextToClipboard(element) {
        var textArea = document.createElement("textarea");
        textArea.value = element.text();
        document.body.appendChild(textArea);
        textArea.select();
        try {
            var successful = document.execCommand('copy');
            if (successful) {
                $('.action-clipboardicon').removeClass('w3-text-green');
                $('#icon' + $(element).attr('id')).addClass('w3-text-green');
            }
            var msg = successful ? 'successful' : 'unsuccessful';
            console.log('Copying text command was ' + msg);
        }
        catch (err) {
            console.log('Oops, unable to copy');
        }
        textArea.remove();

    }

    function moveToTop(selector) {

        var w3modals = $(".w3-modal");
        var max = 0;
        w3modals.each(function () {
            var z = parseInt($(this).css("z-index"), 10);
            max = Math.max(max, z);
        });
        $(selector).css("z-index", max + 1);
        $(selector).show();
        //$('.w3-modal-content').draggable();

    }

    function popupmsg(avatar, heading, message) {
        $('#messagepopupavatar').attr('src', avatar);
        $('#messagepopupheading').text(heading);
        $('#messagepopuphelp').text(message);
        moveToTop('#messagepopup');
        $('#messagepopup').show();
    }

    // #####################################################################

    return {
        moveToTop: moveToTop,
        CopyTextToClipboard: CopyTextToClipboard,
        popupmsg: popupmsg
    }

}();


