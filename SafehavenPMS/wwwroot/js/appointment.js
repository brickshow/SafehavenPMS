// Wait until the DOM is fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Get references to key elements
    const addBtn = document.getElementById("availability-button"); // The button to open Add Availability
    const closeBtn = document.getElementById("close-availability"); // Button to close the panel
    const tabElementList = document.querySelectorAll('button[data-bs-toggle="tab"]'); // All tabs
    const availabilityInput = document.getElementById("availability-input"); // The Add Availability panel

    // -------------------------
    // Restore active tab on page load
    // -------------------------
    const savedTab = localStorage.getItem("activeTab"); // Get saved tab ID from localStorage
    if (savedTab) {
        const tabToActivate = document.querySelector(`button[data-bs-target="${savedTab}"]`);
        if (tabToActivate) {
            const tab = new bootstrap.Tab(tabToActivate);
            tab.show(); // Show saved tab
        }
    }

    // -------------------------
    // Show/hide add button when tabs change & save active tab
    // -------------------------
    tabElementList.forEach(function (tabElement) {
        tabElement.addEventListener('shown.bs.tab', function (event) {
            const targetId = event.target.getAttribute("data-bs-target");
            localStorage.setItem("activeTab", targetId); // Save current tab
            if (addBtn) {
                // Show add button only if the current tab is "availability"
                addBtn.style.display = targetId === "#availability" ? "flex" : "none";
            }
        });
    });

    // -------------------------
    // Set initial visibility of add button
    // -------------------------
    const activeTabElement = document.querySelector('button[data-bs-toggle="tab"].active');
    if (addBtn) {
        addBtn.style.display =
            activeTabElement && activeTabElement.getAttribute("data-bs-target") === "#availability"
                ? "flex"
                : "none";
    }

    // -------------------------
    // Open the availability input with animation
    // -------------------------
    if (addBtn) {
        addBtn.addEventListener("click", function (e) {
            e.preventDefault();
            if (availabilityInput) {
                availabilityInput.style.display = "block"; // Show panel
                setTimeout(() => availabilityInput.classList.add("show"), 10); // Add animation class
                localStorage.setItem("availability-input-visible", "true"); // Save visibility in localStorage
            }
        });
    }

    // -------------------------
    // Close the availability input with animation
    // -------------------------
    if (closeBtn) {
        closeBtn.addEventListener("click", function () {
            if (availabilityInput) {
                availabilityInput.classList.remove("show"); // Remove animation
                availabilityInput.addEventListener('transitionend', function handler() {
                    availabilityInput.style.setProperty('display', 'none', 'important'); // Hide panel
                    availabilityInput.removeEventListener('transitionend', handler);
                    localStorage.setItem("availability-input-visible", "false"); // Update localStorage
                });
            }
        });
    }

    // -------------------------
    // Restore availability panel visibility after refresh
    // -------------------------
    if (localStorage.getItem("availability-input-visible") === "true" && availabilityInput) {
        availabilityInput.style.display = "block"; // Show panel
        setTimeout(() => availabilityInput.classList.add("show"), 10); // Add animation
    }

    // -------------------------
    // Persist input values for title, dates, and checkbox
    // -------------------------
    const inputIds = ["availability-title", "start-date", "end-date", "availability-checkbox"];
    inputIds.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            if (el.type === "checkbox") {
                el.checked = JSON.parse(localStorage.getItem(id)) || false; // Restore checkbox state
            } else {
                el.value = localStorage.getItem(id) || ""; // Restore input value
            }

            // Save changes to localStorage
            el.addEventListener('input', function () {
                if (el.type === "checkbox") {
                    localStorage.setItem(id, el.checked);
                } else {
                    localStorage.setItem(id, el.value);
                }
            });
        }
    });

    // -------------------------
    // Handle day checkboxes (show/hide times)
    // -------------------------
    document.querySelectorAll('.day-checkbox').forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            const dayName = this.id.replace('-check', ''); // Get day name from checkbox ID
            const timesDiv = document.getElementById(dayName + '-times'); // Find times container
            timesDiv.style.display = this.checked ? 'block' : 'none'; // Show/hide times
        });
    });

});

// -------------------------
// Function to add a new time slot row for a day
// -------------------------
function addTimeSlot(dayName) {
    const container = document.getElementById(dayName + '-times'); // Container for time slots
    const slotCount = container.querySelectorAll('.d-flex').length + 1; // Count existing slots

    const div = document.createElement('div'); // Create new row
    div.className = 'd-flex align-items-center mb-2';
    div.id = `${dayName}-slot-${slotCount}`;
    div.innerHTML = `
        <input type="time" class="form-control me-2" value="06:00">
        <span class="me-2">–</span>
        <input type="time" class="form-control me-2" value="07:00">
        <button type="button" class="btn btn-sm btn-outline-danger" onclick="this.parentElement.remove()">-</button>
    `;
    container.appendChild(div); // Add row to container
}

// -------------------------
// Gather all availability data from form
// -------------------------
function gatherAvailabilityForSave() {
    const days = [];

    document.querySelectorAll('.day-checkbox').forEach(checkbox => {
        const dayName = checkbox.id.replace('-check', '');
        const dayId = checkbox.dataset.dayId;
        if (!checkbox.checked) return; // Skip unchecked days

        const timeSlots = [];
        document.querySelectorAll(`#${dayName}-times .d-flex`).forEach(slotDiv => {
            const start = slotDiv.querySelector('input[type="time"]:nth-child(1)').value;
            const end = slotDiv.querySelector('input[type="time"]:nth-child(3)').value;
            timeSlots.push({ StartTime: start, EndTime: end });
        });

        days.push({ DayId: dayId, DayName: dayName, TimeSlots: timeSlots });
    });

    return {
        ClinicalStaffID: document.querySelector('input[name="ClinicalStaffID"]').value,
        Title: document.querySelector('#availability-title').value,
        StartDate: document.querySelector('#start-date').value,
        EndDate: document.querySelector('#end-date').value,
        NoEndDate: document.querySelector('#availability-checkbox').checked,
        Days: days
    };
}

// -------------------------
// Save availability to server
// -------------------------
function saveAvailability() {
    const data = gatherAvailabilityForSave();

    // Get the checkbox and input elements
    const noEndDateCheckbox = document.getElementById('availability-checkbox');
    const endDateInput = document.getElementById('end-date');
    const startDateInput = document.getElementById('start-date');

    // -------------------------
    // Required field validations
    // -------------------------
    if (!data.Title || data.Title.trim() === "") {
        showToast('Title is required!', 'error');
        return;
    }

    if (!data.StartDate || data.StartDate.trim() === "") {
        showToast('Start date is required!', 'error');
        return;
    }

    const today = new Date();
    const startDate = new Date(startDateInput.value);

    // -------------------------
    // Start date must not be in past
    // -------------------------
    if (startDate < today.setHours(0, 0, 0, 0)) {
        showToast('Start date cannot be in the past!', 'error');
        return;
    }

    // -------------------------
    // End Date validation
    // -------------------------
    if (!noEndDateCheckbox.checked) { // If "No End Date" is NOT checked
        if (!endDateInput.value || endDateInput.value.trim() === "") {
            showToast("Please select an End Date or check 'No End Date'", 'error');
            return;
        }

        const endDate = new Date(endDateInput.value);
        if (endDate < startDate) {
            showToast("End Date cannot be earlier than Start Date", 'error');
            return;
        }
    }

    // -------------------------
    // Days validation
    // -------------------------
    if (!data.Days || data.Days.length === 0) {
        showToast('Select at least one day before saving!', 'error');
        return;
    }


    // -------------------------
    // Time slot validation for each day
    // -------------------------
    // -------------------------
    // Time slot validation for each day
    // -------------------------
    let invalidTime = false;

    document.querySelectorAll('.day-checkbox').forEach(cb => {
        if (cb.checked) {
            const dayName = cb.id.replace('-check', '');
            const timesContainer = document.getElementById(dayName + '-times');
            if (timesContainer) {
                const timeInputs = timesContainer.querySelectorAll('input[type="time"]');
                const startTimesSet = new Set();

                // Loop through start-end pairs
                for (let i = 0; i < timeInputs.length; i += 2) {
                    const startTime = timeInputs[i].value;
                    const endTime = timeInputs[i + 1]?.value;

                    if (!startTime || !endTime) {
                        invalidTime = true;
                        showToast(`Time slots for ${dayName} cannot be empty`, 'error');
                        return;
                    }

                    // Check for duplicate start times
                    if (startTimesSet.has(startTime)) {
                        invalidTime = true;
                        showToast(`Duplicate start time '${startTime}' for ${dayName} is not allowed`, 'error');
                        return;
                    }
                    startTimesSet.add(startTime);

                    // Convert times to minutes for proper comparison
                    const [startHour, startMinute] = startTime.split(':').map(Number);
                    const [endHour, endMinute] = endTime.split(':').map(Number);
                    const startTotal = startHour * 60 + startMinute;
                    const endTotal = endHour * 60 + endMinute;

                    if (startTotal >= endTotal) {
                        invalidTime = true;
                        showToast(`Invalid time slot for ${dayName}: Start time must be before End time`, 'error');
                        return;
                    }
                }
            }
        }
    });

    if (invalidTime) return;

    // -------------------------
    // All validations passed, send data to server
    // -------------------------
    fetch('/ClinicalStaff/SaveAvailabilityJson', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                showToast('Availability saved successfully!', 'success');

                // -------------------------
                // Hide panel after save
                // -------------------------
                const availabilityDiv = document.getElementById('availability-input');
                if (availabilityDiv) {
                    availabilityDiv.classList.remove('show');
                    availabilityDiv.style.setProperty('display', 'none', 'important');
                }

                // -------------------------
                // Clear all input values & localStorage
                // -------------------------
                const inputIds = ["availability-title", "start-date", "end-date", "availability-checkbox"];
                inputIds.forEach(id => {
                    const el = document.getElementById(id);
                    if (el) {
                        if (el.type === "checkbox") el.checked = false;
                        else el.value = '';
                        localStorage.removeItem(id);
                    }
                });

                // -------------------------
                // Clear day checkboxes, hide times, reset slots, remove localStorage
                // -------------------------
                document.querySelectorAll('.day-checkbox').forEach(cb => {
                    cb.checked = false;
                    const dayName = cb.id.replace('-check', '');
                    const timesContainer = document.getElementById(dayName + '-times');
                    if (timesContainer) {
                        timesContainer.style.display = 'none';
                        timesContainer.innerHTML = `
                            <div class="d-flex align-items-center mb-2" id="${dayName}-slot-1">
                                <input type="time" class="form-control me-2" value="06:00">
                                <span class="me-2">–</span>
                                <input type="time" class="form-control me-2" value="07:00">
                                <button type="button" class="btn btn-sm btn-outline-primary" onclick="addTimeSlot('${dayName}')">+</button>
                            </div>
                        `;
                    }
                    localStorage.removeItem(`${dayName}-check`);
                });

                // -------------------------
                // Remove visibility flag
                // -------------------------
                localStorage.removeItem("availability-input-visible");

            } else {
                const errorMessage = result.message || 'Error saving availability';
                showToast(errorMessage, 'error');
            }
        })
        .catch(err => {
            console.error(err);
            showToast('Server error: ' + (err.message || 'Unable to save availability'), 'error');
        });
}

//Show Toast for notification
function showToast(message, type = 'success') {
    const toastContainer = document.getElementById('toast-container');

    // Create the toast element
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white border-0 ${type === 'success' ? 'bg-success' : 'bg-danger'}`;
    toastEl.role = 'alert';
    toastEl.ariaLive = 'assertive';
    toastEl.ariaAtomic = 'true';
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastContainer.appendChild(toastEl);

    // Show toast using Bootstrap's JS
    const bsToast = new bootstrap.Toast(toastEl, { delay: 3000 });
    bsToast.show();

    // Remove toast from DOM after it's hidden
    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}