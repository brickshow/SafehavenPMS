document.addEventListener('DOMContentLoaded', function () {
    const addBtn = document.getElementById("availability-button");
    const closeBtn = document.getElementById("close-availability");
    const tabElementList = document.querySelectorAll('button[data-bs-toggle="tab"]');

    // Show/hide add button when tabs change
    tabElementList.forEach(function (tabElement) {
        tabElement.addEventListener('shown.bs.tab', function (event) {
            const targetId = event.target.getAttribute("data-bs-target");
            addBtn.style.display = targetId === "#availability" ? "flex" : "none";
        });
    });

    // Set initial visibility of add button
    const activeTab = document.querySelector('button[data-bs-toggle="tab"].active');
    addBtn.style.display =
        activeTab && activeTab.getAttribute("data-bs-target") === "#availability"
            ? "flex"
            : "none";

    // Open the availability input when clicking the add button
    if (addBtn) {
        addBtn.addEventListener("click", function (e) {
            e.preventDefault();
            const availabilityInput = document.getElementById("availability-input");
            if (availabilityInput) {
                availabilityInput.style.display = "block";
            }
        });
    }

    // Close the availability input when clicking the close button
    if (closeBtn) {
        closeBtn.addEventListener("click", function () {
            const availabilityInput = document.getElementById("availability-input");
            if (availabilityInput) {
                availabilityInput.style.setProperty('display', 'none', 'important');            }
        });
    }
});


//function updateWeekdays() {
//    // Get the start date input value
//    const startDate = document.getElementById('start-date').value;
//    // Get the end date input value
//    const endDate = document.getElementById('end-date').value;
//    // Check if "No End Date" checkbox is checked
//    const noEndDate = document.getElementById('availability-checkbox').checked;
//    // Get the container element where weekdays will be rendered
//    const weekdaysContainer = document.getElementById('weekdays-container');

//    // Clear previous weekday sections to refresh the UI
//    weekdaysContainer.innerHTML = '';

//    // If start date is empty, show error message and return
//    if (!startDate) {
//        document.getElementById('start-date-error').textContent = 'Start date is required';
//        return;
//    } else {
//        // Clear error message if start date is provided
//        document.getElementById('start-date-error').textContent = '';
//    }

//    let end;

//    // Calculate the end date based on "No End Date" checkbox
//    if (noEndDate) {
//        // If "No End Date" is checked, set end date to one month after start date
//        end = new Date(startDate);
//        end.setMonth(end.getMonth() + 1);
//        // Clear end date error and disable end date input
//        document.getElementById('end-date-error').textContent = '';
//        document.getElementById('end-date').disabled = true;
//        document.getElementById('end-date').value = '';
//    } else {
//        // Enable end date input
//        document.getElementById('end-date').disabled = false;

//        // If end date is missing, show error and return
//        if (!endDate) {
//            document.getElementById('end-date-error').textContent = 'End date is required unless "No End Date" is checked';
//            return;
//        }

//        end = new Date(endDate);

//        // If end date is before start date, show error and return
//        if (end < new Date(startDate)) {
//            document.getElementById('end-date-error').textContent = 'End date must be after start date';
//            return;
//        } else {
//            // Clear end date error if validation passes
//            document.getElementById('end-date-error').textContent = '';
//        }
//    }

//    // Array of all weekdays as strings (lowercase)
//    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];

//    // Set to store weekdays that fall within the date range
//    const activeDays = new Set();

//    // Determine active weekdays within the date range
//    const start = new Date(startDate);

//    // Loop from start date up to and including end date
//    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
//        // Add the weekday string (e.g. "monday") to activeDays set
//        activeDays.add(days[d.getDay()]);
//    }

//    // Loop over all days of the week to create UI sections
//    days.forEach(day => {
//        // Check if current day is within the date range
//        const isActive = activeDays.has(day);

//        // Create a container div for this day's section
//        const daySection = document.createElement('div');
//        daySection.className = `day-section mb-3 ${isActive ? 'active' : ''}`; // Add active class if day is in range
//        daySection.id = `${day}-section`; // Set unique id for the day section

//        // Set inner HTML for the day section
//        daySection.innerHTML = `
//            <div class="form-check d-flex align-items-center">
//                <input class="form-check-input me-2" type="checkbox" id="${day}-check" onchange="toggleDay(this, '${day}')" ${isActive ? '' : 'disabled'}>
//                <label class="form-check-label fw-semibold" for="${day}-check">${day.charAt(0).toUpperCase() + day.slice(1)}</label>
//            </div>
//            <div class="time-slots mt-2" id="${day}-times" style="display: none;">
//                <div class="d-flex align-items-center mb-2" id="${day}-slot-1">
//                    <input type="time" class="form-control me-2" value="06:00">
//                    <span class="me-2">–</span>
//                    <input type="time" class="form-control me-2" value="07:00">
//                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="addTimeSlot('${day}')">+</button>
//                </div>
//            </div>
//        `;

//        // Append the created day section to the weekdays container in the DOM
//        weekdaysContainer.appendChild(daySection);

//        // Get the checkbox input for the current day
//        const checkbox = document.getElementById(`${day}-check`);

//        // If the day is not active (outside date range), uncheck the checkbox and hide its time slots
//        if (!isActive) {
//            checkbox.checked = false;
//            document.getElementById(`${day}-times`).style.display = 'none';
//        }
//    });
//}

//function toggleDay(checkbox, day) {
//    const timeSlotsDiv = document.getElementById(`${day}-times`);
//    if (checkbox.checked) {
//        timeSlotsDiv.style.display = 'block';
//    } else {
//        timeSlotsDiv.style.display = 'none';
//    }
//}

//function addTimeSlot(day) {
//    const timeSlotsDiv = document.getElementById(`${day}-times`);
//    const slotCount = timeSlotsDiv.children.length + 1;

//    const newSlot = document.createElement('div');
//    newSlot.className = 'd-flex align-items-center mb-2';
//    newSlot.id = `${day}-slot-${slotCount}`;
//    newSlot.innerHTML = `
//        <input type="time" class="form-control me-2" value="06:00">
//        <span class="me-2">–</span>
//        <input type="time" class="form-control me-2" value="07:00">
//        <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeTimeSlot('${day}-slot-${slotCount}')">-</button>
//    `;

//    timeSlotsDiv.appendChild(newSlot);
//}

//function removeTimeSlot(slotId) {
//    const slot = document.getElementById(slotId);
//    if (slot) {
//        slot.remove();
//    }
//}

//// Function to collect all form data and send to server
//function saveAvailability() {
//    // Clear any existing error messages
//    clearErrorMessages();

//    // Get basic form data
//    const title = document.getElementById('availability-title').value.trim();
//    const startDate = document.getElementById('start-date').value;
//    const endDate = document.getElementById('end-date').value;
//    const noEndDate = document.getElementById('availability-checkbox').checked;
//    const clinicalStaffId = parseInt(document.getElementById('clinical-staff-id').value);

//    // Validate basic fields
//    let hasErrors = false;

//    if (!title) {
//        document.getElementById('title-error').textContent = 'Title is required';
//        hasErrors = true;
//    }

//    if (!startDate) {
//        document.getElementById('start-date-error').textContent = 'Start date is required';
//        hasErrors = true;
//    }

//    if (!noEndDate && !endDate) {
//        document.getElementById('end-date-error').textContent = 'End date is required unless "No End Date" is checked';
//        hasErrors = true;
//    }

//    // Collect selected days and their time slots
//    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
//    const selectedDays = [];

//    days.forEach(day => {
//        const checkbox = document.getElementById(`${day}-check`);
//        if (checkbox && checkbox.checked) {
//            const timeSlotsDiv = document.getElementById(`${day}-times`);
//            const timeSlots = [];

//            // Get all time slot pairs for this day
//            const slotDivs = timeSlotsDiv.querySelectorAll('.d-flex.align-items-center.mb-2');
//            slotDivs.forEach(slotDiv => {
//                const timeInputs = slotDiv.querySelectorAll('input[type="time"]');
//                if (timeInputs.length === 2) {
//                    const startTime = timeInputs[0].value;
//                    const endTime = timeInputs[1].value;

//                    if (startTime && endTime && endTime > startTime) {
//                        timeSlots.push({
//                            startTime: startTime,
//                            endTime: endTime
//                        });
//                    }
//                }
//            });

//            if (timeSlots.length > 0) {
//                selectedDays.push({
//                    dayName: day.charAt(0).toUpperCase() + day.slice(1), // Capitalize first letter
//                    timeSlots: timeSlots
//                });
//            }
//        }
//    });

//    if (selectedDays.length === 0) {
//        alert('Please select at least one day with time slots');
//        hasErrors = true;
//    }

//    if (hasErrors) {
//        return;
//    }

//    // Prepare data to send
//    const requestData = {
//        title: title,
//        startDate: startDate,
//        endDate: noEndDate ? null : endDate,
//        noEndDate: noEndDate,
//        clinicalStaffId: clinicalStaffId,
//        days: selectedDays
//    };

//    // Show loading state
//    const saveButton = document.querySelector('button[type="submit"]');
//    const originalText = saveButton.textContent;
//    saveButton.disabled = true;
//    saveButton.textContent = 'Saving...';

//    // Send to server
//    fetch('/Appointment/AddAvailabilityDate', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
//        },
//        body: JSON.stringify(requestData)
//    })
//        .then(response => response.json())
//        .then(data => {
//            if (data.success) {
//                // Success - close modal and show success message
//                const modal = bootstrap.Modal.getInstance(document.getElementById('calendar-modal'));
//                modal.hide();

//                // You can show a success toast/alert here
//                showToast('Availability saved successfully!', 'success');

//                // Reset form
//                resetForm();
//            } else {
//                // Show error message
//                showToast(data.message || 'An error occurred while saving.', 'error');
//            }
//        })
//        .catch(error => {
//            console.error('Error:', error);
//            showToast('An error occurred while saving. Please try again.', 'error');
//        })
//        .finally(() => {
//            // Restore button state
//            saveButton.disabled = false;
//            saveButton.textContent = originalText;
//        });
//}

//function clearErrorMessages() {
//    document.getElementById('title-error').textContent = '';
//    document.getElementById('start-date-error').textContent = '';
//    document.getElementById('end-date-error').textContent = '';
//}

//function resetForm() {
//    document.getElementById('availability-form').reset();
//    document.getElementById('weekdays-container').innerHTML = '';
//    clearErrorMessages();
//}


//    // Handle form submission
//    document.getElementById('availability-form').addEventListener('submit', function (e) {
//        e.preventDefault(); // Prevent default form submission
//        saveAvailability();
//    });

//    // Initialize weekdays when modal is shown
//    const calendarModal = document.getElementById('calendar-modal');
//    if (calendarModal) {
//        calendarModal.addEventListener('shown.bs.modal', function () {
//            updateWeekdays();
//        });
//    }


////Show Toast for notification
//function showToast(message, type = 'success') {
//    const toastContainer = document.getElementById('toast-container');

//    // Create the toast element
//    const toastEl = document.createElement('div');
//    toastEl.className = `toast align-items-center text-white border-0 ${type === 'success' ? 'bg-success' : 'bg-danger'}`;
//    toastEl.role = 'alert';
//    toastEl.ariaLive = 'assertive';
//    toastEl.ariaAtomic = 'true';
//    toastEl.innerHTML = `
//        <div class="d-flex">
//            <div class="toast-body">
//                ${message}
//            </div>
//            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
//        </div>
//    `;

//    toastContainer.appendChild(toastEl);

//    // Show toast using Bootstrap's JS
//    const bsToast = new bootstrap.Toast(toastEl, { delay: 3000 });
//    bsToast.show();

//    // Remove toast from DOM after it's hidden
//    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
//}