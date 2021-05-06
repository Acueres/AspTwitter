window.functions = {
    openSelector: function () {
        document.getElementById('avatar-selector').click();
    },

    loadAvatar: function () {
        let avatar = document.getElementById("avatar-selector");

        let reader = new FileReader();

        let mb = 1024 * 1024;

        if (avatar.files && avatar.files[0] && avatar.files[0].size <= mb) {
            reader.onload = (e) => {
                let result = e.target.result;
                document.getElementById("avatar").src = result;
            }

            reader.readAsDataURL(avatar.files[0]);
        }
    },

    uploadAvatar: function (id, token, server) {
        let avatar = document.getElementById("avatar-selector");
        let mb = 1024 * 1024;

        if (avatar.files && avatar.files[0] && avatar.files[0].size <= mb) {
            let image = avatar.files[0];
            let form = new FormData();
            form.append("avatar", image);

            let settings = {
                "async": true,
                "crossDomain": true,
                "url": server + `api/users/${id}/avatar`,
                "method": "POST",
                "processData": false,
                "contentType": false,
                "mimeType": "multipart/form-data",
                "data": form,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", 'Bearer ' + token);
                }
            };

            jQuery.ajax(settings);
        }
    }
}