// site.js

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar toggle functionality
    const sidebar = document.querySelector('.sidebar');
    const menuToggle = document.querySelector('.bx-menu');

    if (menuToggle && sidebar) {
        menuToggle.addEventListener('click', () => {
            sidebar.classList.toggle('close');
        });
    }

    // Navigation active state handler + submenu toggle on click
    const navigationItems = document.querySelectorAll('.navigation');

    // Prevent submenu link clicks from toggling parent
    document.querySelectorAll('.submenu a').forEach(link => {
        link.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    });

    navigationItems.forEach(item => {
        item.addEventListener('click', function (e) {
            // If this item has a submenu, toggle it; otherwise just set active
            const hasSubmenu = !!this.querySelector('.submenu');
            const wasActive = this.classList.contains('active');
            const directLink = this.querySelector(':scope > a[href]');

            // Close all others first
            navigationItems.forEach(navItem => navItem.classList.remove('active'));

            if (hasSubmenu) {
                if (wasActive) {
                    // It was open; keep it closed now
                    if (directLink) {
                        e.preventDefault();
                    }
                    return;
                }
                // It was closed; open it now
                this.classList.add('active');
                if (directLink) {
                    e.preventDefault();
                }
            } else {
                this.classList.add('active');
            }
        });
    });

    // Set active state based on current URL
    const currentPath = window.location.pathname.toLowerCase();
    navigationItems.forEach(item => {
        const link = item.querySelector('a');
        if (link && link.getAttribute('href').toLowerCase() === currentPath) {
            item.classList.add('active');
        }
    });
});