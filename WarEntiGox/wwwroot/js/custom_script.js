document.addEventListener('DOMContentLoaded', function () {
    // Tooltip başlatma
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Tüm dropdown öğelerine dinleyici ekleyin
    var dropdowns = document.querySelectorAll('.nav-item.dropdown');

    dropdowns.forEach(function (dropdown) {
        var menu = dropdown.querySelector('.dropdown-menu');

        // Menü açılma olayını fareyle üzerine gelindiğinde başlat
        dropdown.addEventListener('mouseenter', function () {
            dropdown.classList.add('show');
            menu.classList.add('show');
        });

        // Menü kapanma olayını fare menüden çıkınca başlat
        dropdown.addEventListener('mouseleave', function (event) {
            // Eğer fare menünün dışında bir yere giderse menüyü kapat
            if (!dropdown.contains(event.relatedTarget)) {
                dropdown.classList.remove('show');
                menu.classList.remove('show');
            }
        });

        // Menü öğelerine tıklanabilirlik ekleyin (opsiyonel)
        menu.addEventListener('click', function (e) {
            // Menü öğesine tıklanırsa menü kapanmaz
            e.stopPropagation(); // Tıklama olayını menüyü kapatmadan engelle
        });
    });
});
