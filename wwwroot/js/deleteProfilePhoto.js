document.getElementById('resetProfilePhotoCheckbox').addEventListener('change', function (event) {
    if (event.target.checked) {
        // Retrieve the userId from the data attribute
        const userId = event.target.getAttribute('data-user-id');

        if (!userId) {
            console.error('User ID not found.');
            alert('Error: User ID is missing.');
            return;
        }

        fetch('/Home/ResetProfilePhoto', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(userId) // Send the user ID as a JSON string
        })
            .then(response => response.json())
            .then(data => {
                if (data.message) {
                    alert(data.message);
                    // Optionally update the UI to reflect the change
                } else if (data.error) {
                    alert('Error: ' + data.error);
                }
            })
            .catch(error => console.error('Error:', error));
    }
});
