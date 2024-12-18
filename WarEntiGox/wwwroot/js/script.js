document.addEventListener("DOMContentLoaded", function() {
    const menuItems = document.querySelectorAll('.navbar > ul > li > a');

    menuItems.forEach(item => {
        item.addEventListener('click', function(e) {
            const subMenu = item.nextElementSibling;

            // Eğer bir alt menü varsa
            if (subMenu && subMenu.classList.contains('sub-menu')) {
                e.preventDefault(); // Menü tıklama olayını engeller

                // Alt menü zaten açıksa, kapat, kapalıysa aç
                if (subMenu.classList.contains('show')) {
                    subMenu.classList.remove('show');
                } else {
                    subMenu.classList.add('show');
                }
            }
        });
    });
});
