using Discord.Addons.ShaneInteractive.Pagination;
using System.Threading.Tasks;

namespace example
{
    public sealed class CustomPaginator : Paginator
    {
        public CustomPaginator(PaginatorBuilder builder) : base(builder) { }

        // Changing the order of reactions
        protected override async Task AddReactionsAsync()
        {
            await AddReactionAsync(Reactions.Info);
            await AddReactionAsync(Reactions.Front);
            await AddReactionAsync(Reactions.Rear);
            await AddReactionAsync(Reactions.Previous);
            await AddReactionAsync(Reactions.Stop);
            await AddReactionAsync(Reactions.Next);
            await AddReactionAsync(Reactions.Trash);
            await AddReactionAsync(Reactions.Jump);
        }
    }
}