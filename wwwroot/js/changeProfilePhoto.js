function triggerFileInput() {
    // Trigger the hidden file input when the button is clicked
    document.getElementById('profilePhotoFile').click();
}

function submitChangePhotoForm() {
    // Automatically submit the form when a file is selected
    document.getElementById('changePhotoForm').submit();
}

document.addEventListener("DOMContentLoaded", function () {
    const resetPhotoButton = document.getElementById("resetProfilePhotoButton");

    if (resetPhotoButton) {
        resetPhotoButton.addEventListener("click", async function () {
            const userId = resetPhotoButton.getAttribute("data-user-id");

            if (!userId) {
                alert("User ID is missing.");
                return;
            }

            const confirmation = confirm("Are you sure you want to reset the profile photo?");
            if (!confirmation) {
                return;
            }

            try {
                const response = await fetch(`/Users/ResetProfilePhoto`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify(userId),
                });

                if (response.ok) {
                    alert("Profile photo reset successfully.");
                    location.reload(); // Reload the page to reflect changes
                } else {
                    const errorData = await response.json();
                    alert(errorData.error || "Failed to reset profile photo.");
                }
            } catch (error) {
                console.error("Error resetting profile photo:", error);
                alert("An error occurred while resetting the profile photo.");
            }
        });
    }
});