mergeInto(LibraryManager.library, {
    requestPointerLock: function () {
        document.addEventListener('click', function () {
            document.body.requestPointerLock = document.body.requestPointerLock ||
                document.body.mozRequestPointerLock ||
                document.body.webkitRequestPointerLock;
            document.body.requestPointerLock();
        }, { once: true });
    },

    exitPointerLock: function () {
        document.exitPointerLock = document.exitPointerLock ||
            document.mozExitPointerLock ||
            document.webkitExitPointerLock;
        document.exitPointerLock();
    }
});