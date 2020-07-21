const languageSelect = document.getElementById('Language');
const selectedValue = languageSelect.value;
const messageContainer = document.getElementById('message-container');

languageSelect.addEventListener('change', event => {
    messageContainer.innerHTML = '';
    if (selectedValue !== event.target.value) {
        let messageDiv = document.createElement('DIV');
        messageDiv.classList = 'message info';
        messageDiv.innerText = window.languageChangedWarning;
        messageContainer.appendChild(messageDiv);
    } else {
        messageContainer.innerHTML = '';
    }
});

const saveProfileButton = document.getElementById('save-profile-button');
const changeProfileImageLabel = document.getElementById('change-profile-image-label');
const removeImageButton = document.getElementById('remove-image-button');
const imageUriInput = document.getElementById('ImageUri');
const profileImage = document.getElementById('profile-image');
const profileImageOverlay = document.getElementById('profile-image-overlay');
const imageInput = document.getElementById('file-input');

imageInput.addEventListener('change', () => {
    imageInput.setAttribute('disabled', 'disabled');
    changeProfileImageLabel.className = 'disabled';
    removeImageButton.setAttribute('disabled', 'disabled');
    saveProfileButton.setAttribute('disabled', 'disabled');

    const imageFile = imageInput.files[0];
    if (imageFile.size > 5 * 1024 * 1024 && imageFile.size < 10 * 1024 * 1024) {
        messageContainer.innerHTML = '';
        let messageDiv = document.createElement('DIV');
        messageDiv.classList = 'message info';
        messageDiv.innerHTML = window.largeImageWarning;
        messageContainer.appendChild(messageDiv);
    }

    profileImageOverlay.style.opacity = 1;

    const formData = new FormData();
    formData.append('image', imageFile);

    fetch(window.baseUrl + '/account/upload-profile-image', {
        method: 'post',
        body: formData
    })
        .then(response => {
            response.json().then(data => {
                if (response.status !== 201) {
                    let errorsArray = '';

                    if (response.status === 500) {
                        errorsArray = data['An unexpected error occurred'];
                    } else if (response.status === 422) {
                        errorsArray = data[''];
                    }

                    messageContainer.innerHTML = '';
                    let messageDiv = document.createElement('DIV');
                    messageDiv.classList = 'message danger';
                    messageDiv.innerText = errorsArray.join(' ');
                    messageContainer.appendChild(messageDiv);
                } else {
                    messageContainer.innerHTML = '';
                    imageUriInput.value = profileImage.src = data.imageUri;

                    removeImageButton.className = '';
                    saveProfileButton.removeAttribute('disabled');
                }

                removeImageButton.removeAttribute('disabled');
                imageInput.removeAttribute('disabled');
                changeProfileImageLabel.className = '';
                profileImageOverlay.style.opacity = 0;
            });
        });
});

removeImageButton.addEventListener('click', () => {
    profileImage.src = window.defaultImageUri;
    removeImageButton.className = 'hidden';
    imageUriInput.value = '';
});