var edit = new Vue({
    el: '#edit',

    data:
    {
        originalName: user.name,
        about: user.about,
        nameInvalid: false
    },

    methods:
    {
        save: async function () {
            let nameField = document.getElementById('editName');

            let name = nameField.value;

            this.nameInvalid = name == '';

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
                    about: this.about
                })
            });

            if (response.status === 200) {
                user.name = name;
                user.about = this.about;
                user.updateStorage();

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit'));
                modal.toggle();
            }
        }
    },

    computed:
    {
        charactersLeft: function () {
            if (this.about == null || this.about.length == 0) {
                return null;
            }

            return this.about.length + '/' + '160';
        }
    }
});