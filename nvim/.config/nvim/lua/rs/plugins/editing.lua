return {
    {
        'windwp/nvim-autopairs',
        event = { "BufReadPre", "BufNewFile" },
        opts = {
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
