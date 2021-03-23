var edit = new Vue({
    el: '#edit',

    data:
    {
        user: user,
        nameInvalid: false,
        usernameInvalid: false,

        usernameMessage: '',

        errorMessages: {
            usernameEmpty: 'Username can\'t be empty',
            usernameExists: 'Username already exists',
        }
    },

    methods:
    {
        save: async function () {
            let nameField = document.getElementById('edit-name');
            let usernameField = document.getElementById('edit-username');
            let aboutField = document.getElementById('about');

            let name = nameField.value;
            let username = usernameField.value;
            let about = aboutField.value;

            this.nameInvalid = name == '';
            this.usernameInvalid = username == '';

            this.usernameMessage = this.errorMessages.usernameEmpty;

            const response = await fetch(`http://localhost:5000/api/users/${user.id}`, {
                method: 'PUT',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + user.token
                },
                body: JSON.stringify({
                    name: name,
                    username: username,
                    about: about
                })
            });

            if (response.status === 200) {
                user.name = name;
                user.username = username;
                user.about = about;

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit'));
                modal.toggle();
            }
            else if (response.status == 409) {
                this.usernameInvalid = true;
                this.usernameMessage = this.errorMessages.usernameExists;
            }
        },

        reset: function () {
            document.getElementById('edit-name').value = user.name;
            document.getElementById('edit-username').value = user.username;
            document.getElementById('about').value = user.about;
        }
    }
});