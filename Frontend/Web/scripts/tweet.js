var tweet = new Vue({
    el: '#tweet',
    data:
    {
        entry: null,
        user: user,
        text: '',
        comments: []
    },

    methods:
    {
        load: async function (entry) {
            this.entry = entry;

            const response = await fetch(`http://localhost:5000/api/entries/${this.entry.id}/comments`, {
                method: 'GET',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow'
            });

            this.comments = await response.json();
        },

        reply: async function () {
            const response = await fetch(`http://localhost:5000/api/entries/${this.entry.id}/comments`, {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + user.token
                },
                body: JSON.stringify({ authorId: user.id, text: this.text })
            });

            const responseData = await response.json();

            if (response.status == 200) {
                this.comments.push({
                    id: responseData,
                    author: user,
                    authorId: user.id,
                    text: String(this.text)
                });

                this.entry.commentCount++;
                this.text = '';
            }
        },

        deleteComment: async function (id) {
            const response = await fetch(`http://localhost:5000/api/entries/${this.entry.id}/comments/${id}`, {
                method: 'DELETE',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + user.token
                }
            });

            if (response.status == 200) {
                let index = this.comments.findIndex((e) => e.id == id);
                if (index != -1) {
                    this.comments.splice(index, 1);
                }

                this.entry.commentCount--;
            }
        },

        getAvatar: function (id) {
            return `http://localhost:5000/api/users/${id}/avatar`;
        }
    }
});