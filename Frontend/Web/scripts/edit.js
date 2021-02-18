var edit = new Vue({
    el: '#edit',

    data:
    {
        nameInvalid: false,
        name: JSON.parse(localStorage.getItem('currentUser')).name,
        aboutText: JSON.parse(localStorage.getItem('currentUser')).about
    },

    methods:
    {
        save: async function () {
            let nameField = document.getElementById('editName');

            let name = nameField.value;

            this.nameInvalid = name == '';

            const currentUser = JSON.parse(localStorage.getItem('currentUser'));

            const response = await fetch(`http://localhost:5000/api/users/${currentUser.id}`, {
                method: 'PUT',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + currentUser.token
                },
                body: JSON.stringify({
                    name: name,
                    about: this.aboutText
                })
            });

            if (response.status === 200) {
                currentUser.name = name;
                currentUser.about = this.aboutText;
                localStorage.setItem('currentUser', JSON.stringify(currentUser));

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit'));
                modal.toggle();
            }
        },

        uploadImage: function () {
            let imageInput = document.getElementById("imageInput");
            let avatar = document.getElementById("editAvatar");

            let reader = new FileReader();

            if (imageInput.files && imageInput.files[0]) {
                reader.onload = (e) => {
                    avatar.setAttribute('src', e.target.result);
                }
                reader.readAsDataURL(imageInput.files[0]);

                let image = document.getElementById("imageInput").files[0];
                let form = new FormData();
                form.append("avatar", image);

                const currentUser = JSON.parse(localStorage.getItem('currentUser'));

                let settings = {
                    "async": true,
                    "crossDomain": true,
                    "url": `http://localhost:5000/api/users/${currentUser.id}/avatar`,
                    "method": "POST",
                    "processData": false,
                    "contentType": false,
                    "mimeType": "multipart/form-data",
                    "data": form,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", 'Bearer ' + currentUser.token);
                    }
                };

                jQuery.ajax(settings);
            }
        }
    },

    computed:
    {
        charactersLeft: function () {
            if (this.aboutText.length == 0) {
                return null;
            }

            return this.aboutText.length + '/' + '160';
        },

        avatar: async function () {

            const id = JSON.parse(localStorage.getItem('currentUser')).id;

            const response = await fetch(`http://localhost:5000/api/users/${id}/avatar`);
            const image = await response.blob();

            let avatar = document.getElementById("editAvatar");

            var reader = new FileReader();
            reader.onload = (e) => {
                avatar.src = e.target.result;
            };

            reader.readAsDataURL(image);
        }
    }
});