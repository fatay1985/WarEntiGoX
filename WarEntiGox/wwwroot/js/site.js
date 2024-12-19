// Ensure that Bootstrap dropdowns work as intended, including for submenus
$(document).ready(function () {
    $('.dropdown-submenu a.dropdown-toggle').on("click", function (e) {
        var $subMenu = $(this).next(".dropdown-menu");
        if ($subMenu.is(":visible")) {
            $subMenu.hide();
        } else {
            $(".dropdown-menu").not($subMenu).hide(); // Hide other open menus
            $subMenu.show();
        }
        e.stopPropagation();
        e.preventDefault();
    });
});
