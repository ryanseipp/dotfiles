return {
    {
        'windwp/nvim-autopairs',
        event = { "BufReadPre", "BufNewFile" },
        config = {
            check_ts = true,
        }
    },
    {
        'windwp/nvim-ts-autotag',
        event = { "BufReadPre", "BufNewFile" },
    },
    {
        'numToStr/Comment.nvim',
        event = { "BufReadPre", "BufNewFile" },
        config = true,
    },
}
