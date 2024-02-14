local lazypath = vim.fn.stdpath("data") .. "/lazy/lazy.nvim"
if not vim.loop.fs_stat(lazypath) then
    vim.fn.system({
        "git",
        "clone",
        "--filter=blob:none",
        "https://github.com/folke/lazy.nvim.git",
        "--branch=stable", -- latest stable release
        lazypath,
    })
end
vim.opt.rtp:prepend(lazypath)

vim.g.mapleader = ' '

require('lazy').setup('rs.plugins')

vim.keymap.set('x', '<leader>p', "\"_dp")

-- :help <opt>
vim.opt.cmdheight = 1
vim.opt.showmatch = true
vim.opt.relativenumber = true
vim.opt.number = true
vim.opt.ignorecase = true
vim.opt.smartcase = true
vim.opt.hidden = true
vim.opt.cursorline = true
vim.opt.equalalways = true
vim.opt.splitright = true
vim.opt.splitbelow = true
vim.opt.updatetime = 1000
vim.opt.scrolloff = 10

vim.opt.autoindent = true
vim.opt.cindent = true
vim.opt.wrap = true

vim.opt.tabstop = 4
vim.opt.shiftwidth = 4
vim.opt.softtabstop = 4
vim.opt.expandtab = true
vim.opt.textwidth = 120

vim.opt.breakindent = true
vim.opt.linebreak = true

vim.opt.foldmethod = "marker"
vim.opt.foldlevel = 0
vim.opt.modelines = 1

vim.opt.belloff = "all"

vim.opt.clipboard = "unnamedplus"

vim.opt.inccommand = "split"
vim.opt.shada = { "!", "'1000", "<50", "s10", "h" }

vim.opt.mouse = "a"

vim.opt.formatoptions = vim.opt.formatoptions
    - "a"
    - "t"
    + "c"
    + "q"
    - "o"
    + "r"
    + "n"
    + "j"
    - "2"
