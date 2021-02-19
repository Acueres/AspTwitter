var profile = new Vue({
    el: '#profile',

    data:
    {
        name: JSON.parse(localStorage.getItem('currentUser')).name,
        username: JSON.parse(localStorage.getItem('currentUser')).username,
        about: JSON.parse(localStorage.getItem('currentUser')).about,
        logged: isLogged(),
        entries: []
    },

    created: async function () {
        const id = JSON.parse(localStorage.getItem('currentUser')).id;

        if (id != '') {
            const response = await fetch(`http://localhost:5000/api/users/${id}/entries`);
            const data = await response.json();
            this.entries = data;
            this.entries.reverse();
        }

    },

    methods:
    {
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
        avatar: async function () {
            const id = JSON.parse(localStorage.getItem('currentUser')).id;

            if (id != '') {
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
    }
});