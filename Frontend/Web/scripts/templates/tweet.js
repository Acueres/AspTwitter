var tweet = {
    props: ['entry', 'index', 'avatar', 'user', 'deleteEntry'],
    template: `
    <div class="row border-bottom p-3">
      <div class="col-auto">
        <img v-bind:src='avatar' class="img rounded-circle"
          style="width: 50px; height: 50px; outline: none;" alt="avatar">
      </div>

      <div class="col-auto">
        <b style="font-size: large;">{{ entry.author.name }}</b> @{{ entry.author.username }}

        <button type="button" class="btn btn-default" aria-label="Left Align"
          v-on:click='deleteEntry(index, entry.id)' v-if="entry.author.id == user.id">
          <span class="la la-trash" aria-hidden="true"></span>
        </button> <br>
        <a style="color: black;">{{ entry.text }}</a> <br>

        <div class="btn-group" role="group" aria-label="Basic example">
          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align" v-if="user.logged">
            <span class="la la-comment-dots" aria-hidden="true"></span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align" v-if="user.logged">
            <span class="la la-retweet" aria-hidden="true"></span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align" v-if="user.logged">
            <span class="la la-heart" aria-hidden="true"></span>
          </button>
        </div>

      </div>
    </div>`
}