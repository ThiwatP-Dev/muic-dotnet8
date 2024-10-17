const mainMenuClass = "sidebar-main-menu";
const subMenuClass = "sidebar-sub-menu";

const getSidebarMenu = function () {
    
    var menuLocal = localStorage.getItem('TokenMenu');
    var expiredToken = getWithExpiry('ExpiredToken');
    var decode = atob(menuLocal);

    if(expiredToken !== null && menuLocal != null) {
        var menu = JSON.parse(decode);
        SetMenu(menu);
    } else {

        var ajax = new AJAX_Helper(
            {
                url: "/api/Menu/GetMenu",
                contentType: "application/json"
            }
        );

        ajax.GET().done(function (response) { 
            var menu = JSON.stringify(response);
            var encode = btoa(menu)
            setWithExpiry('ExpiredToken', 'Expired', 1800000);
            localStorage.setItem('TokenMenu', encode);
            SetMenu(response);
        })
        .fail(function (jqXHR, textStatus, errorThrown) { 
            // ErrorCallback(jqXHR, textStatus, errorThrown);
        });
    }
}

function setWithExpiry(key, value, ttl) {
    const now = new Date()

    // `item` is an object which contains the original value
    // as well as the time when it's supposed to expire
    const item = {
        value: value,
        expiry: now.getTime() + ttl,
    }
    localStorage.setItem(key, JSON.stringify(item))
}

function getWithExpiry(key) {
    const itemStr = localStorage.getItem(key)

    // if the item doesn't exist, return null
    if (!itemStr) {
        return null
    }

    const item = JSON.parse(itemStr)
    const now = new Date()

    // compare the expiry time of the item with the current time
    if (now.getTime() > item.expiry) {
        // If the item is expired, delete the item from storage
        // and return null
        localStorage.removeItem(key)
        return null
    }
    return item.value
}

// get sidebar menu
getSidebarMenu();

// open sidebar function & close all menu when shrink sidebar
$("#sidebar-toggler").on("click", function () {
    let subMenuList = $("#sidebar-menu-list").find(`.${subMenuClass}`);
    let mainMenuList = $("#sidebar-menu-list").find(`.${mainMenuClass}`);
    let menuList = $.merge(subMenuList, mainMenuList);
    menuList.each( function() {
        let isOpen = $(this).find(".navbar-toggler").attr("aria-expanded");
        isOpen == "true" && $(this).find(".navbar-toggler").trigger("click");
    });

    $("body").toggleClass("sidebar-expand");
    $("#toggle-sidebar").toggleClass("expanded");
});

// force open sidebar when click menu while sidebar still shrink & close all submenus when close main menu
$(document).on("click", `.${mainMenuClass}`, function() {
    let isExpanded = $("body").hasClass("sidebar-expand");
    if (!isExpanded) {
        $("body").toggleClass("sidebar-expand");
        $("#toggle-sidebar").toggleClass("expanded");
    }

    let childrenList = $(this).nextUntil(`.${mainMenuClass}`);
    let isOpenMain = $(this).find(".navbar-toggler").attr("aria-expanded");

    childrenList.each( function() {
        if ($(this).hasClass(subMenuClass)) {
            let isOpenSub = $(this).find(".navbar-toggler").attr("aria-expanded");
            if (isOpenMain == "false" && isOpenSub == "true") {
                $(this).find(".navbar-toggler").trigger("click");
            }
        }
    });
})

function SetMenu(response)
{
    let sidebarMenuArea = $("#sidebar-menu-list");
    response.forEach(mainMenu => {

        let mainRef = mainMenu.header.replace(RegularExpressions.AllWhitespace, '');

        sidebarMenuArea.append(`
            <li class="nav-item ${mainMenuClass}">
                <a class="nav-link" 
                href=".${mainRef}-submenus"
                role="button" 
                data-toggle="collapse"
                aria-expanded="false" 
                aria-controls="${mainRef}-submenus">
                    <i class="${mainMenu.icon}"></i>
                    <span class="menu-title">${mainMenu.header}</span>
                </a>
                <button class="navbar-toggler collapsed ${mainMenu.submenus.length > 0 ? "" : "d-none"}" 
                        data-toggle="collapse" 
                        data-target=".${mainRef}-submenus" 
                        aria-expanded="false" 
                        aria-controls="${mainRef}-submenus">
                    <i class="fas fa-angle-down"></i>
                </button>
            </li>
        `);

        mainMenu.submenus.length > 0 && mainMenu.submenus.forEach(subMenu => {
            let subRef = subMenu.submenus_header.replace(RegularExpressions.AllWhitespace, '');
            sidebarMenuArea.append(`
                <li class="${mainRef}-submenus nav-sub-item collapse ${subMenuClass}">
                    <a class="nav-link"
                    href=".${mainRef}-${subRef}-items"
                    role="button" 
                    data-toggle="collapse"
                    aria-expanded="false" 
                    aria-controls="${mainRef}-${subRef}-items">
                        <span class="menu-title">${subMenu.submenus_header}</span>
                    </a>
                    <button class="navbar-toggler collapsed ${subMenu.submenus_items.length > 0 ? "" : "d-none"}" 
                            data-toggle="collapse" 
                            data-target=".${mainRef}-${subRef}-items" 
                            aria-expanded="false" 
                            aria-controls="${mainRef}-${subRef}-items">
                        <i class="fas fa-angle-down"></i>
                    </button>
                </li>
            `);

            subMenu.submenus_items.length > 0 && subMenu.submenus_items.forEach(menuItem => {
                sidebarMenuArea.append(`
                    <li class="${mainRef}-${subRef}-items nav-sub-item collapse">
                        <a href="${menuItem.url}" class="nav-link-item ml-3">
                            <span class="menu-title">${menuItem.title}</span>
                        </a>
                    </li>
                `);
            })
        })
    });

    sidebarMenuArea.append(`
        <li class="">
            <a id="button-logout" href="/Account/Logout" class="nav-link-item">
                <i class="fas fa-door-open"></i>
                <span class="menu-title">Logout</span>
            </a>
        </li>
    `);

    // open a menu that corresponding current link
    $(".nav-link-item").each( function() {
        let currentPath = window.location.pathname.split('/');
        let itemPath = $(this)[0].pathname.split('/');
        if (currentPath[1] == itemPath[1]) {
            let parentItem = $(this).parent();
            parentItem.addClass("active");

            let prevSubMenus = parentItem.prevAll(`.${subMenuClass}`);
            $(prevSubMenus[0]).find(".navbar-toggler").trigger("click");

            let mainSubMenus = parentItem.prevAll(`.${mainMenuClass}`);
            $(mainSubMenus[0]).find(".navbar-toggler").trigger("click");
        }
    })
}