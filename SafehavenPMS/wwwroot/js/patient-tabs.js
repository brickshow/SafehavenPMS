document.addEventListener('DOMContentLoaded', function () {
    //Get the tab id's
    const tabs = document.querySelectorAll('.nav-link');
    //Get the id od content sections
    const tabContent = document.getElementById('tabContentArea');

    //function to load the content of the tab
    function loadTabContent(url, link) {
        fetch(url)
            .then(response => response.text())
            .then(html => {
                tabContent.innerHTML = html; // Update the content area with the fetched HTML
                // Update the active class on the tabs
                tabs.forEach(tab => tab.classList.remove('active'));
                link.classList.add('active');
            })
    }

    // Add click event listeners to each tab
    tabs.forEach(tab => {
        tab.addEventListener('click', function (event) {
            event.preventDefault(); // Prevent the default anchor behavior
            const url = this.getAttribute('data-url'); // Get the URL from the href attribute
            if(url)loadTabContent(url, this); // Load the content for the clicked tab
        });
    });

    //Load the content when active tab is clicked
    const activeTab = document.querySelector('.nav-link.active');
    if (activeTab) {
        const activeUrl = activeTab.getAttribute('href');
        if (activeUrl) {
            loadTabContent(activeUrl, activeTab);
        }
    }
});