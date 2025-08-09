document.addEventListener('DOMContentLoaded', function () {
    // Existing code for tab handling
    const addBtn = document.getElementById("availability-button");
    const tabElementList = document.querySelectorAll('button[data-bs-toggle="tab"]');

    tabElementList.forEach(function (tabElement) {
        tabElement.addEventListener('shown.bs.tab', function (event) {
            const targetId = event.target.getAttribute("data-bs-target");
            addBtn.style.display = targetId === "#availability" ? "flex" : "none";
        });
    });

    const activeTab = document.querySelector('button[data-bs-toggle="tab"].active');
    addBtn.style.display = activeTab && activeTab.getAttribute("data-bs-target") === "#availability" ? "flex" : "none";
});