var profile = new Vue({
    el: '#profile',

    data:
    {
        user: user,
        entries: entries
    },

    created: async function () {
        if (user.id != null) {
            await entries.loadUserEntries(user.id);
        }

        //Update edit modal default values when changing user
        jQuery('#edit').on('show.bs.modal', function () {
            edit.name = user.name;
            edit.about = user.about;
        });
    },

    methods:
    {
        deleteEntry: async function (index, id) {

            const response = await fetch(`http://localhost:5000/api/entries/${id}`, {
                method: 'DELETE',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + user.token
                }
            });

            if (response.status == 200) {
                entries.deleteFromProfile(index, id);
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

                let settings = {
                    "async": true,
                    "crossDomain": true,
                    "url": `http://localhost:5000/api/users/${user.id}/avatar`,
                    "method": "POST",
                    "processData": false,
                    "contentType": false,
                    "mimeType": "multipart/form-data",
                    "data": form,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", 'Bearer ' + user.token);
                    }
                };

                jQuery.ajax(settings);
            }
        }
    },

    computed:
    {
        avatar: async function () {
            if (user.id != null) {
                const response = await fetch(`http://localhost:5000/api/users/${user.id}/avatar`);

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