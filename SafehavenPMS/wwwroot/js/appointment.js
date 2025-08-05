
//Adding all javascript code in display calendar
document.addEventListener('DOMContentLoaded', function () {

    //Instantiate atrributes by getting element ID
    const dateInput = document.getElementById("appointmentDate");
    const availableTimeContainer = document.getElementById("availableTimes");
    const noTimeMessage = document.getElementById("noTimesMsg");

    //Flatpickr Initialization
    flatpickr(dateInput, {
        minDate: "today",
        onchange: function (selectedDates, dateStr) {
            fetch(`/Appointment/GetAvailableTimes?date=${dateStr}`)
                .then(res => res.json())
                .then(availableTimes => {
                    //Clear old buttons
                    availableTimeContainer.innerHTML = "";

                    //Check if there is available time and diplay yit as a button
                    if (availableTimes.length > 0) {
                        availableTimes.forEach(time => {

                            //create button
                            const btn = document.createElement("button");
                            btn.className = "btn btn-outline-secondary time-btn";
                            btn.textContent = time;
                        });
                    }
                })
        }
    });
});