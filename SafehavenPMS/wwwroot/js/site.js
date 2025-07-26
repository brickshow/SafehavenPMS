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

    // Navigation active state handler
    const navigationItems = document.querySelectorAll('.navigation');
    
    navigationItems.forEach(item => {
        item.addEventListener('click', function() {
            // Remove active class from all navigation items
            navigationItems.forEach(navItem => {
                navItem.classList.remove('active');git add
            });
            
            // Add active class to clicked item
            this.classList.add('active');
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