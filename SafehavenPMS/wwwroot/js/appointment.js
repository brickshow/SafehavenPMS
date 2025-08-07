
//Adding all javascript code in availability and apponment 
document.addEventListener('DOMContentLoaded', function () {
    // Get the "Add Availability" button by ID
    const addBtn = document.getElementById("availability-button");

    // Find all tab buttons that toggle tabs
    const tabElementList = document.querySelectorAll('button[data-bs-toggle="tab"]');

    //Loop trough each tab button
    tabElementList.forEach(function (tabElement) {
        //Listen for when a tab is click by the user
        tabElement.addEventListener('shown.bs.tab', function (event) {
            const targetId = event.target.getAttribute("data-bs-target");

            //Show/Hide the Add avalability button 
            if (targetId === "#availability") {
                addBtn.style.display = "flex";
            } else {
                addBtn.style.display = "none";
            }
        });
    });

    // Check initially if the Availability tab is already active on page load
    const activeTab = document.querySelector('button[data-bs-toggle="tab"].active');
    if (activeTab && activeTab.getAttribute("data-bs-target") === "#availability") {
        addBtn.style.display = "flex";
    } else {
        addBtn.style.display = "none";
    }
});

// Triggered when the start or end date changes
function updateWeekdays() {
    const startDate = document.getElementById('start-date').value;
    const endDate = document.getElementById('end-date').value;
    const weekdaysContainer = document.getElementById('weekdays-container');

    // Clear previous weekday sections
    weekdaysContainer.innerHTML = '';

    // Stop if start or end date is missing
    if (!startDate || !endDate) return;

    const start = new Date(startDate);
    const end = new Date(endDate);

    // Validate end date must be after or equal to start date
    if (end < start) {
        document.getElementById('end-date-error').textContent = 'End date must be after start date';
        return;
    } else {
        document.getElementById('end-date-error').textContent = '';
    }

    // List of weekdays in order
    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
    const uniqueDays = new Set();

    // Collect all unique weekdays between the date range
    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        uniqueDays.add(days[d.getDay()]);
    }

    // Render checkboxes and default time slot for each available weekday
    uniqueDays.forEach(day => {
        const daySection = document.createElement('div');
        daySection.className = 'day-section mb-3';
        daySection.id = `${day}-section`;

        daySection.innerHTML = `
            <div class="form-check d-flex align-items-center">
                <input class="form-check-input me-2" type="checkbox" id="${day}-check" onchange="toggleDay(this, '${day}')">
                <label class="form-check-label fw-semibold" for="${day}-check">${day.charAt(0).toUpperCase() + day.slice(1)}</label>
            </div>
            <div class="time-slots mt-2" id="${day}-times" style="display: none;">
                <div class="d-flex align-items-center mb-2" id="${day}-slot-1">
                    <input type="time" class="form-control me-2" value="06:00">
                    <span class="me-2">–</span>
                    <input type="time" class="form-control me-2" value="07:00">
                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="addTimeSlot('${day}')">+</button>
                </div>
            </div>
        `;
        weekdaysContainer.appendChild(daySection);
    });
}

// Show or hide time slots based on checkbox status
function toggleDay(checkbox, day) {
    const timeSlots = document.getElementById(`${day}-times`);
    timeSlots.style.display = checkbox.checked ? 'block' : 'none';
}

// Add a new time slot for a specific weekday
function addTimeSlot(day) {
    const timeSlots = document.getElementById(`${day}-times`);
    const slotCount = timeSlots.children.length + 1;

    const newSlot = document.createElement('div');
    newSlot.className = 'd-flex align-items-center mb-2';
    newSlot.id = `${day}-slot-${slotCount}`;

    newSlot.innerHTML = `
        <input type="time" class="form-control me-2" value="06:00">
        <span class="me-2">–</span>
        <input type="time" class="form-control me-2" value="07:00">
        <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeTimeSlot('${day}', ${slotCount})">-</button>
    `;

    timeSlots.appendChild(newSlot);
}

// Remove a specific time slot by ID
function removeTimeSlot(day, slotId) {
    const slot = document.getElementById(`${day}-slot-${slotId}`);
    if (slot) slot.remove();
}

// Gather all data and log it (or send to server)
function saveAvailability() {
    const title = document.getElementById('availability-title').value;
    const startDate = document.getElementById('start-date').value;
    const endDate = document.getElementById('end-date').value;
    const availability = {};

    // Validate required fields
    if (!title || !startDate || !endDate) {
        if (!title) document.getElementById('title-error').textContent = 'Title is required';
        if (!startDate) document.getElementById('start-date-error').textContent = 'Start date is required';
        if (!endDate) document.getElementById('end-date-error').textContent = 'End date is required';
        return;
    }

    // Collect all active (checked) weekdays and their time slots
    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
    days.forEach(day => {
        const checkbox = document.getElementById(`${day}-check`);
        if (checkbox && checkbox.checked) {
            const timeSlots = document.getElementById(`${day}-times`);
            const slots = [];

            timeSlots.querySelectorAll('.d-flex').forEach(slot => {
                const times = slot.querySelectorAll('input[type="time"]');
                slots.push({
                    start: times[0].value,
                    end: times[1].value
                });
            });

            availability[day] = slots;
        }
    });

    // Log collected data (you can replace this with an AJAX request to send to server)
    console.log({
        title,
        startDate,
        endDate,
        availability
    });
}


//// Enable/disable weekdays based on date range
//function updateWeekdays() {
//    const startDate = document.getElementById('start-date').value;
//    const endDate = document.getElementById('end-date').value;
//    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];

//    // Reset all checkboxes to disabled
//    days.forEach(day => {
//        const checkbox = document.getElementById(`${day}-check`);
//        checkbox.disabled = true;
//        document.getElementById(`${day}-times`).style.display = 'none';
//    });

//    if (!startDate || !endDate) return;

//    const start = new Date(startDate);
//    const end = new Date(endDate);
//    if (end < start) {
//        document.getElementById('end-date-error').textContent = 'End date must be after start date';
//        return;
//    } else {
//        document.getElementById('end-date-error').textContent = '';
//    }

//    // Enable checkboxes for days in the date range
//    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
//        const dayIndex = d.getDay();
//        const day = days[dayIndex];
//        document.getElementById(`${day}-check`).disabled = false;
//    }
//}

//// Show/hide time slots when checkbox is toggled
//function toggleDay(checkbox, day) {
//    document.getElementById(`${day}-times`).style.display = checkbox.checked ? 'block' : 'none';
//}

//// Save availability
//function saveAvailability() {
//    const title = document.getElementById('availability-title').value;
//    const startDate = document.getElementById('start-date').value;
//    const endDate = document.getElementById('end-date').value;
//    const availability = {};

//    if (!title) {
//        document.getElementById('title-error').textContent = 'Title is required';
//        return;
//    } else {
//        document.getElementById('title-error').textContent = '';
//    }
//    if (!startDate) {
//        document.getElementById('start-date-error').textContent = 'Start date is required';
//        return;
//    } else {
//        document.getElementById('start-date-error').textContent = '';
//    }
//    if (!endDate) {
//        document.getElementById('end-date-error').textContent = 'End date is required';
//        return;
//    } else {
//        document.getElementById('end-date-error').textContent = '';
//    }

//    const days = ['sunday', 'monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday'];
//    days.forEach(day => {
//        const checkbox = document.getElementById(`${day}-check`);
//        if (checkbox && checkbox.checked) {
//            const times = document.getElementById(`${day}-times`).querySelectorAll('input[type="time"]');
//            availability[day] = [{
//                start: times[0].value,
//                end: times[1].value
//            }];
//        }
//    });

//    console.log({
//        clinicalStaffId: '@staff?.ClinicalStaffID',
//        title,
//        startDate,
//        endDate,
//        availability
//    });

//    // Optional: Send data to server
//    /*
//    fetch('/ClinicalStaff/SaveAvailability', {
//        method: 'POST',
//        headers: { 'Content-Type': 'application/json' },
//        body: JSON.stringify({
//            clinicalStaffId: '@staff?.ClinicalStaffID',
//            title,
//            startDate,
//            endDate,
//            availability
//        })
//    }).then(response => response.json())
//      .then(data => {
//          if (data.success) {
//              document.getElementById('calendar-modal').classList.remove('show');
//              document.body.classList.remove('modal-open');
//              document.querySelector('.modal-backdrop').remove();
//          } else {
//              alert('Error saving availability');
//          }
//      });
//    */
//}