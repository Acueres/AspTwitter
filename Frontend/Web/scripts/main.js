var main = new Vue(
    {
        el: '#main',

        data:
        {
            entries: []
        },

        methods:
        {
            get: async function () {
                const response = await fetch('http://localhost:5000/api/entries');
                const data = await response.json();
                this.entries = data;
            }
        }
    });

main.get();