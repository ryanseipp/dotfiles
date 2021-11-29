local M = {}

function M.map(mode, lhs, rhs, opts)
    if not opts then
        opts = {}
    end

    vim.tbl_extend('force', opts, {noremap = true})
    vim.api.nvim_set_keymap(mode, lhs, rhs, opts)
end

function M.map_buf(buf, mode, lhs, rhs, opts)
    if not opts then
        opts = {}
    end

    vim.tbl_extend('force', opts, {noremap = true})
    vim.api.nvim_buf_set_keymap(buf, mode, lhs, rhs, opts)
end

return M
