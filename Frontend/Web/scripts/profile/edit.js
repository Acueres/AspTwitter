var edit = new Vue({
    el: '#edit',

    data:
    {
        user: user,
        nameInvalid: false
    },

    methods:
    {
        save: async function () {
            let nameField = document.getElementById('editName');
            let aboutField = document.getElementById('about');

            let name = nameField.value;
            let about = aboutField.value;

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
                    about: about
                })
            });

            if (response.status === 200) {
                user.name = name;
                user.about = about;

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit'));
                modal.toggle();
            }
        },

        reset: function () {
            document.getElementById('editName').value = user.name;
            document.getElementById('about').value = user.about;
        }
    }
});