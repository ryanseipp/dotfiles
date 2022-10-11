local M = {}

local function bind(mode, outer_opts)
    outer_opts = outer_opts or { noremap = true }
    return function(lhs, rhs, opts)
        opts = vim.tbl_extend("force", outer_opts, opts or {})
        vim.keymap.set(mode, lhs, rhs, opts)
    end
end

local function bind_buf(mode, outer_opts)
    outer_opts = outer_opts or { noremap = true }
    return function(bufnr, lhs, rhs, opts)
        opts = vim.tbl_extend("force", outer_opts, opts or {})
        vim.api.nvim_buf_set_keymap(bufnr, mode, lhs, rhs, opts)
    end
end

M.nmap = bind("n", { noremap = false })
M.nnoremap = bind("n")
M.vnoremap = bind("v")
M.xnoremap = bind("x")
M.inoremap = bind("i")

M.buf_nmap = bind_buf("n", { noremap = false })
M.buf_nnoremap = bind_buf("n")
M.buf_vnoremap = bind_buf("v")
M.buf_xnoremap = bind_buf("x")
M.buf_inoremap = bind_buf("i")

return M
